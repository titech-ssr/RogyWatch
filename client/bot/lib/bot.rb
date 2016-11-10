require 'websocket-client-simple'
require 'timeout'

module RogyWatch

  class Bot

    
    def initialize(ws_conf)
      @ws_conf = ws_conf
    end

    def connect
      @std, @std_handshake = handshake(@ws_conf[:std][:address], @ws_conf[:std][:port], ->(){ puts "std Opend" })
      sleep 1
      @err, @err_handshake = handshake(@ws_conf[:err][:address], @ws_conf[:err][:port], ->(){ puts "err Opend" })

    end

    def handshake(host, port, opened)

      target = TCPSocket.new(host, port)
      handshake = WebSocket::Handshake::Client.new(url: "ws://#{host}:#{port}")

      open = Thread.new do 
        loop {
          recv = target.getc
          print recv
          handshake << recv
          if handshake.finished? # open
            opened.()
            break
          end
        }
      end

      sleep 0.5

      target.write handshake.to_s
      puts "handshaked sent"
      open.join

      return target, handshake
    end

    def send(line, type: :text)

      frame = WebSocket::Frame::Outgoing::Client.new(data: line, type: type, version: @std_handshake.version)
      begin
        @std.write frame.to_s
      rescue Errno::EPIPE => e
        $stderr.puts "#{e.message}\n#{e.backtrace}"
      end

    end

    def read(timeout)
      frame = ::WebSocket::Frame::Incoming::Client.new
      threads = {}

      Timeout.timeout(timeout) {

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
          result
        end

        threads[:err] = Thread.new do
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
          Exception.new(result)
        end

        threads[:std].join
        threads.each{|k,n| n.kill}

        threads.select{|k,v| v.value}.map do |k,v|
          if WebSocket::Frame::Incoming::Client === (val = v.value)
            val
          else
            raise val
          end
        end.first
      }
    end

    def close
      send(nil, type: :close)
      begin
        frame = WebSocket::Frame::Outgoing::Client.new(data: "close", type: :close, version: @err_handshake.version)
        @err.write frame.to_s
      rescue Errno::EPIPE => e
        $stderr.puts "#{e.message}\n#{e.backtrace}"
      end
      @std.close
      @err.close
    end


  end

end

