#!/usr/bin/env ruby
# coding: utf-8

require 'pathname'
$: << Pathname(__FILE__).dirname.expand_path

require 'yaml'
require 'pp'
require 'date'

require 'bundler'
require 'lib/tools'
require 'lib/bot'


ScreenName  = :rogy_watch
cert = (config = YAML.load_file("config.yaml"))[ScreenName]
Admin = config[ScreenName][:admin]


rest = authoricate(screen_name:ScreenName, type: :REST, keys:cert)
stream = authoricate(screen_name:ScreenName, type: :Streaming, keys:cert)
cert = nil

ws_conf = config[:server][:ws]
bot = RogyWatch::Bot.new(ws_conf)


puts "start streaming"

begin
stream.user{|status|

  case status

    when Twitter::Tweet then
      puts "#{status.user.screen_name}\n#{status.text} #{(reply = status.text =~ /@#{ScreenName}/) && status.user.id == Admin} #{reply}"
      content = status.text.gsub(/@#{ScreenName}/, "").strip

      if (reply = status.text =~ /@#{ScreenName}/) && status.user.id == Admin then
        now = "#{(d = DateTime.now).year}_#{d.month}_#{d.day}_#{d.hour}_#{d.second}"
        Thread.new{
          begin
            p result = eval(content)
            rest.update("@#{status.user.screen_name} #{result}\n#{d.to_s}", in_reply_to_status: status)
          rescue => e
            rest.update(
              "@#{status.user.screen_name} #{e.message}\n#{d.to_s}", 
              in_reply_to_status: status) rescue puts("#{$!.message}\n#{$!.backtrace}")
              puts e.message,e.backtrace
          end
        }
      elsif reply
        now = "#{(d = DateTime.now).year}_#{d.month}_#{d.day}_#{d.hour}_#{d.second}"
        Thread.new{
          begin
            bot.connect(ws_conf[:err][:connection_timeout])

            if    content =~ /how/i then bot.send("HowManyPeople #{now}")
            elsif content =~ /echo/i then bot.send("Echo #{now}")
            else
              bot.send("HowManyPeople #{now}")
              result = bot.read(ws_conf[:timeout]).data.split(/\s+/).map{|n| n.strip}
              rogy_watch(
                rest, status, result[1], result[0], now, 
                config[ScreenName][:media_dirs])
              next
            end

            p result = bot.read(ws_conf[:timeout]).data
            rest.update("@#{status.user.screen_name} #{result}\n#{d.to_s}"[0, 140], in_reply_to_status: status)
          rescue Exception => e
            rest.update("@#{status.user.screen_name} #{e.message}\n#{d.to_s}"[0, 140], 
              in_reply_to_status: status) rescue puts("#{$!.message}\n#{$!.backtrace}")
            puts "@#{status.user.screen_name} #{e.message}\n#{e.backtrace}"
          ensure
            bot.close
          end
        }
      end

    when Twitter::Streaming::Event then
  end

} 
rescue Exception => e
  puts e
end
