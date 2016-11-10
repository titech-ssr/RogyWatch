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

stream.user{|status|

  case status

    when Twitter::Tweet then
      puts "#{status.user.screen_name}\n#{status.text}"
      content = status.text.gsub(/@#{ScreenName}/, "").strip

      if (reply = status.text =~ /@#{ScreenName}/) && status.user.id == Admin then
        Thread.new{
          begin
            p result = eval(content)
            rest.update(
              "@#{status.user.screen_name} #{result}\n#{DateTime.now.to_s}", 
              in_reply_to_status: status)
          rescue => e
            rest.update(
              "@#{status.user.screen_name} #{e.message}\n#{DateTime.now.to_s}", 
              in_reply_to_status: status) rescue puts("#{$!.message}\n#{$!.backtrace}")
          end
        }
      elsif reply
        Thread.new{
          begin
            bot.connect
            bot.send("HowManyPeople #{DateTime.now}")
            p count = bot.read(ws_conf[:timeout]).data
            bot.close
          rescue => e
            rest.update(
              "@#{status.user.screen_name} #{e.message}\n#{DateTime.now.to_s}", 
              in_reply_to_status: status) rescue puts("#{$!.message}\n#{$!.backtrace}")
          end
        }
      end

    when Twitter::Streaming::Event then

  end

}
