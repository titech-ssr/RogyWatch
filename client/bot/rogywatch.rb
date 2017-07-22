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
require 'lib/admin'
require 'lib/nonadmin'


# Rogywatch twitter bot screen name
#
ScreenName  = :rogy_watch
cert = (config = YAML.load_file("config.yaml"))[ScreenName]
# Administrator twitter id
Admin = config[ScreenName][:admin] || []


rest = authenticate(screen_name:ScreenName, type: :REST, keys:cert)
stream = authenticate(screen_name:ScreenName, type: :Streaming, keys:cert)
cert = nil

ws_conf = config[:server][:ws]
bot = RogyWatch::Bot.new(ws_conf)


Thread.new{
  now = DateTime.now
  config[ScreenName][:restart_time] =~ /(\d+):(\d+):(\d+)/

  restart_time = DateTime.new(now.year, now.month, now.day, $1.to_i, $2.to_i, $3.to_i, now.offset)
  time = (restart_time - now)*(3600*24.0)
  time += 3600*24 if time < 0

  puts "wait #{time} seconds, (#{Time.at(time).utc.strftime("%H:%M:%S")})"

  sleep time
  exit 0
}

puts "start streaming"

begin
stream.user{|status|

  case status

    when Twitter::Tweet then
      #puts "#{status.user.screen_name}\n#{status.text} #{(reply = status.text =~ /@#{ScreenName}/) && status.user.id == Admin} #{reply}"

      content = status.text.gsub(/@#{ScreenName}/, "").strip
      # cancel self rep
      next if status.user.screen_name == ScreenName.to_s

      # stopped
      if RogyWatch::Admin.stopped then
        RogyWatch::Admin.handle(rest, status, content)
        next
      end


      # for Admin
      if (reply = status.text =~ /@#{ScreenName}/) && Admin.include?(status.user.id) then
        RogyWatch::Admin.handle(bot, ws_conf, rest, status, content)
      elsif reply
        #if RogyWatch::Admin.emergency then
          #rest.update("@#{status.user.screen_name} #{config[ScreenName][:emergency]}\n#{DateTime.now.to_s}"[0, 140], in_reply_to_status: status)
        #else
          RogyWatch::NonAdmin.handle(bot, config, rest, status, content)
        #end
      end

    when Twitter::Streaming::Event then
  end

} 
rescue Exception => e
  puts e.message, e.backtrace, DateTime.now.to_s
  exit 1
end
