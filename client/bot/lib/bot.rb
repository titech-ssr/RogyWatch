require 'websocket-client-simple'
require 'timeout'

# Rogywatch namespace
#
module RogyWatch

  # use to communicate with sever
  #
  class Bot

    def initialize(ws_conf)
      @ws_conf = ws_conf
    end

    # connect to server. Not only std connection, also try to connect stderr
    #
    # @param [Fixnum] err_timeout specify timeout to connect stderr
    # @return [void]
    def connect(err_timeout)

      @std, @std_handshake = handshake(@ws_conf[:std][:address], @ws_conf[:std][:port], ->(){ puts "std Opend" })
      sleep 1
      Timeout.timeout(err_timeout){
        @err, @err_handshake = handshake(@ws_conf[:err][:address], @ws_conf[:err][:port], ->(){ puts "err Opend" })
      } rescue puts "std err connection #{$!}"

    end

    # connect to remote and send websocket handshake.
    #
    # @param [String] host  remote host
    # @param [Fixnum] port  remote port
    # @param [Proc]   opend invoked when handshaked
    # @return [TCPSocket,WebSocket::Handshake::Client] remote, handshake
    def handshake(host, port, opend)

      target = TCPSocket.new(host, port)
      handshake = WebSocket::Handshake::Client.new(url: "ws://#{host}:#{port}")

      open = Thread.new do 
        loop {
          recv = target.getc
          print recv
          handshake << recv
          if handshake.finished? # open
            opend.()
            break
          end
        }
      end

      sleep 0.5

      target.write handshake.to_s
      puts "handshake sent"
      open.join
      puts "handshaked!!!"

      return target, handshake
    end

    # send data to connected remote
    #
    # @param [String] line  data
    # @param [Symbol] type  data type. :text, :close, and so on. {http://qiita.com/south37/items/6f92d4268fe676347160}
    # @return [void]
    def send(line, type: :text)

      frame = WebSocket::Frame::Outgoing::Client.new(data: line, type: type, version: @std_handshake.version)
      begin
        @std&.write frame.to_s
      rescue Errno::EPIPE => e
        $stderr.puts "#{e.message}\n#{e.backtrace}"
      end

    end


    # read data from remote as WebSocket::Frame::Incoming::Client
    #
    # @param  [Fixnum] timeout  read timeout
    # @raise  Timeout::Error
    # @raise  Exception when stderr received data
    # @return [WebSocket::Frame::Incoming::Client]
    # @note if std and stderr connected, remote sent data using stderr raise Exception and Exception#message is set as "remote sent data"
    def read(timeout)
      frame = ::WebSocket::Frame::Incoming::Client.new
      threads = {}

      return Timeout.timeout(timeout) {

        threads[:std] = Thread.new do
          result =  catch(:return){
            loop do
              recv = @std.getc
              frame << recv
              while msg = frame.next
                throw :return, msg
              end
            end
          }
          threads[:err]&.kill
          puts "std!", threads[:err]&.status.inspect, "#{result}\n"
          result
        end

        threads[:err] = Thread.new {
          if @err.nil? then
            nil
          else
            result =  catch(:return){
              loop do
                recv = @err.getc
                frame << recv
                while msg = frame.next
                  throw :return, msg.data
                end
              end
            }
            threads[:std]&.kill
            $stderr.puts "std err!", "#{result}\n"
            Exception.new(result)
          end
        }

        threads[:std].join

        ret = threads.select{|k,v| v&.value}.map do |k,v|
          if WebSocket::Frame::Incoming::Client === (val = v&.value)
            val
          else
            raise val
          end
        end.first
        ret
      }
    end

    # close connection
    #
    # @return [void]
    def close
      send(nil, type: :close)
      begin
        frame = WebSocket::Frame::Outgoing::Client.new(data: "close", type: :close, version: @err_handshake.version)
        @err&.write frame.to_s
      rescue Errno::EPIPE => e
        $stderr.puts "#{e.message}\n#{e.backtrace}"
      end
      @std&.close
      @err&.close
    end


  end

end

