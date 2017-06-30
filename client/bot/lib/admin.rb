require 'open3'

module RogyWatch
  module Admin
    extend self

    class <<
      attr_accessor :stopped, :emergency
    end
    self.stopped   = false
    self.emergency = false

    public

    def start(dummy = 0)
      "Started ..."
    end

    def stop(dummy = 0)
      self.stopped = true
    end

    def emergency(dummy = 0)
      self.emergency = true
    end

    def handle(bot, ws_conf, rest, status, content)
      self.stopped = false if self.stopped and content =~ /start/
      return if self.stopped

      d = DateTime.now
      Thread.new{
        begin
          puts "EVAL #{ result = self.class_eval(content) }"
          rest.update("@#{status.user.screen_name} #{result}\n#{d.to_s}", in_reply_to_status: status)
        rescue => e
          rest.update(
            "@#{status.user.screen_name} #{e.message}\n#{d.to_s}", 
            in_reply_to_status: status) rescue puts("#{$!.message}\n#{$!.backtrace}")
            puts e.message,e.backtrace
        end
      }
    end

    def reload(dummy = 0)
      files = Dir.glob('lib/*.rb')
      files.each{|file| load file }
      return files
    end

    def help(dummy = 0)
      <<-"HELP"
start
stop
update
reload
help
HELP
    end

    def update(dummy = 0)
      `git pull`
      exit 0
    end

  end
end

