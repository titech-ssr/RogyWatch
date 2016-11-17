#!/usr/bin/env ruby
# coding:utf-8

require 'yaml'
require 'timeout'
require 'socket'
require 'digest'

require 'em-websocket'
require 'websocket'

config = YAML.load_file("config.yaml")[:server]
std = config[:ws][:std]
err = config[:ws][:err]


@std = nil
@err = nil

# dummy server for test
#
EM::WebSocket.start(host: std[:address], port: std[:port]) { |con|

    con.onopen do
      @std = con

      Thread.new{
        begin
          Timeout.timeout(err[:timeout]){
            puts "Wait for err #{err[:address]}:#{err[:port]}"
            server = TCPServer.new(err[:address], err[:port])
            $socket = server.accept
            puts "accepted"
            http_request = ""
            while (line = $socket.gets) && (line != "\r\n")
              http_request += line 
            end
            puts http_request

            puts key = http_request[/^Sec-WebSocket-Key: (\S+)/, 1], ""
            response_key = Digest::SHA1.base64digest([key, "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"].join)

            puts response = <<-"EOF"
HTTP/1.1 101 Switching Protocols\r
Upgrade: websocket\r
Connection: Upgrade\r
Sec-WebSocket-Accept: #{ response_key }\r
\r
            EOF
            $socket.write response


            @err = Object.new
            class << @err
              def send(s)
                s.size > 126
                s = s[0,125]
                output = [0b1000_0001, s.size, s]
                puts "SEND"
                p re = output.pack("CCA#{s.size}")
                $socket.write re
              end
            end
            puts "err established"

            Thread.new do
              frame = WebSocket::Frame::Incoming::Server.new
              close = WebSocket::Frame::Data.new("\x88")
              catch(:break) {
                loop do
                  recv = $socket.getc
                  frame << recv

                  if frame.data[0] == close then
                    $socket.close
                    server.close
                    throw :break
                  end
                end
              }
            end
          }

        rescue
          puts "Err output no connnection"
        end
      }

    end

    con.onmessage do |msg|
        begin
          puts (r = eval(msg))
          @std.send(r.to_s)
        rescue => e
          puts ex = "#{e.message}\n#{e.backtrace.join("\n")}"
          (@err || @std).send(ex)
        end
    end

    con.onclose do 
      puts "Close"
    end
}
