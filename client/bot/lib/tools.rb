require 'yaml'
require 'pp'
require 'bundler'
require 'twitter'


def authoricate(screen_name:nil, type:nil, keys:nil)

  raise ArgumentError, "screen_name or type or keys missing" unless screen_name and type and keys


  auth = Proc.new do |config|
    config.consumer_key        = keys[:consumer_key]
    config.consumer_secret     = keys[:consumer_secret]
    config.access_token        = keys[:access_token] 
    config.access_token_secret = keys[:access_token_secret]
  end

  if type == :Streaming then
    client = Twitter::Streaming::Client.new(&auth)
  elsif type == :REST then
    client = Twitter::REST::Client.new(&auth)
  else
    raise ArgumentError, "Unknown type: #{type}"
  end

end


def upload(files)
  media_ids = []
  files.each do |media_filename|
    media_ids << rest.upload(File.new(media_filename))
  end
  media_ids
end


def rogy_watch(rest, status, life_area, work_area, filename, dirs, type: :png)
  media_ids = upload(dirs.map{|dir| "#{dir}/#{filename}.#{type}"})
  tweet = <<-"EOF"
@#{status.user.screen_name} 
生活領域人#{life_area}人くらい、
工作領域人#{work_area}人くらいかも？
EOF
  rest.update(
    tweet,
    in_reply_to_status: status,
    media_ids: media_ids.join(','))
end
