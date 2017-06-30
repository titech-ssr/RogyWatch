module RogyWatch
  module NonAdmin
    extend self

    public

    def handle(bot, ws_conf, rest, status, content)
      now = "#{(d = DateTime.now).year}_#{d.month}_#{d.day}_#{d.hour}_#{d.second}"
      Thread.new{
        begin
          bot.connect(ws_conf[:std][:connection_timeout], ws_conf[:err][:connection_timeout])

          if    content =~ /^how /i then bot.send("HowManyPeople #{now}")
          elsif content =~ /^echo /i then bot.send("Echo #{content.sub(/^echo /i, "")}")
          elsif content =~ /^fizz /i then bot.send("")
          elsif content =~ /^isprime /i then bot.send("")
          elsif content =~ /^sl/i then 
            #ids = upload(["sl.gif"], rest)
            rest.update("@#{status.user.screen_name}"[0, 140], 
            in_reply_to_status: status,
            media_ids: rest.upload(File.new("sl.gif")))
            next
          else
            bot.send("HowManyPeople #{now}")
            result = bot.read(ws_conf[:timeout]).data.split(/\s+/).map{|n| n.strip}
            rogy_watch(
              rest, status, result[1], result[0], now, 
              config[ScreenName][:media_dirs])
            next
          end

          p result = bot.read(ws_conf[:timeout]).data
          puts "result = #{result}"
          rest.update("@#{status.user.screen_name} #{result}\n#{d.to_s}"[0, 140], in_reply_to_status: status)
        rescue Exception => e
          rest.update("@#{status.user.screen_name} 停電ぽい?#{e.message}\n#{d.to_s}"[0, 140], 
            in_reply_to_status: status) rescue puts("#{$!.message}\n#{$!.backtrace}")
          puts "@#{status.user.screen_name} #{e.message}\n#{e.backtrace}"
        ensure
          bot.close
        end
      }
    end

  end
end
