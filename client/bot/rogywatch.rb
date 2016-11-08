#!/usr/bin/env ruby
# coding: utf-8

require 'pathname'
$: << Pathname(__FILE__).dirname.expand_path

require 'yaml'
require 'pp'
require 'date'

require 'bundler'
require 'lib/auth'
require 'websocket-client-simple'


ScreenName  = :rogy_watch
Admin       = "Mizuho32"

cert = (config = YAML.load_file("config.yaml"))[ScreenName]

rest = authoricate(screen_name:ScreenName, type: :REST, keys:cert)
stream = authoricate(screen_name:ScreenName, type: :Streaming, keys:cert)
cert = nil

ws_conf = config[:server][:ws]
ws = WebSocket::Client::Simple.connect("ws://#{ws_conf[:std][:address]}:#{ws_conf[:std][:port]}")

ws.on :message do |msg|
  puts msg.data
end

ws.on :open do
  #ws.send 'hello!!!'
end

ws.on :close do |e|
  p e
  exit 1
end

stream.user{|status|
  case status
    when Twitter::Tweet then
      puts "#{status.user.screen_name}\n#{status.text}"
      if status.text =~ /@#{ScreenName}/ && status.user.screen_name == Admin then
        c = status.text.gsub(/@#{ScreenName}/, "").strip
        Thread.new{
          begin
            p result = eval(c)
            rest.update("@#{status.user.screen_name} #{result}\n#{DateTime.now.to_s}", in_reply_to_status: status)
          rescue => e
            puts e
          end
        }
      end
    when Twitter::Streaming::Event then

  end
}
