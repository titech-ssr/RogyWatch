#!/usr/bin/env ruby

require 'open3'
require 'date'

include Open3

Dir::chdir ENV["HOME"]
Dir::chdir "RogyWatch/client/bot"
tmux_name = "bot"

puts "ruby #{RUBY_VERSION}"

unless capture3("tmux ls").first.include?(tmux_name)
  puts "Start tmux"
  puts capture3("tmux new -d -s #{tmux_name}")
end

sleep 1

puts "Start rogywatch"
puts capture3(%Q{tmux split-window -h -t #{tmux_name}:0 "./rogywatch.sh"})
#puts capture3(%Q{tmux split-window -h -t #{tmux_name}:0 "ruby ./test.rb"})

while true
  puts DateTime.now
  sleep 3600
end
