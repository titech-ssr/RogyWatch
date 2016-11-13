#!/usr/bin/env ruby
# coding: utf-8

require 'pathname'
$: << Pathname(__FILE__).dirname.expand_path

require 'yaml'
require 'pp'
require 'date'

require 'bundler'
require 'lib/auth'
require 'lib/bot'


ScreenName  = :rogy_watch
cert = (config = YAML.load_file("config.yaml"))[ScreenName]
Admin       = config[ScreenName][:admin]


rest = authoricate(screen_name:ScreenName, type: :REST, keys:cert)
stream = authoricate(screen_name:ScreenName, type: :Streaming, keys:cert)
cert = nil

ws_conf = config[:server][:ws]
bot = RogyWatch::Bot.new(ws_conf)

=begin
loop{
bot.connect
begin
  bot.send("#{gets.chomp}")
  p result = bot.read(ws_conf[:readtimeout]).data
rescue Exception => e
  puts e.message, e.backtrace
end
bot.close
}
exit 
=end

stream.user{|status|

  case status

    when Twitter::Tweet then
      puts "#{status.user.screen_name}\n#{status.text}"
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
          end
        }
      elsif reply
        now = "#{(d = DateTime.now).year}_#{d.month}_#{d.day}_#{d.hour}_#{d.second}"
        Thread.new{
          bot.connect(ws_conf[:err][:connection_timeout])
          begin
            if    content =~ /how/i then bot.send("HowManyPeople #{now}")
            elsif content =~ /echo/i then bot.send("Echo #{now}")
            elsif content.size < 100 then bot.send(content)
            else
              rest.update("@#{status.user.screen_name} you said #{content}\n#{d.to_s}", in_reply_to_status: status)
              next
            end
            p result = bot.read(ws_conf[:timeout]).data
            rest.update("@#{status.user.screen_name} #{result}\n#{d.to_s}"[0, 140], in_reply_to_status: status)
          rescue Exception => e
            rest.update("@#{status.user.screen_name} #{e.message}\n#{d.to_s}"[0, 140], 
              in_reply_to_status: status) #rescue puts("#{$!.message}\n#{$!.backtrace}")
            puts "@#{status.user.screen_name} #{e.message}\n#{e.backtrace}"
          end
          bot.close
        }
      end

    when Twitter::Streaming::Event then

  end

}
