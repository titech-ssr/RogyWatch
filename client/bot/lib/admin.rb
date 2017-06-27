require 'open3'

module RogyWatch
  module Admin
    extend self

    public

    def handle(rest, status, content)
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

