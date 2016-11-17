require 'yaml'
require 'pp'
require 'bundler'
require 'twitter'

# returns authenticate rest/streaming object
#
# @option  [String]               screen_name   (nil) screen name
# @option  [Symbol]               type          (nil) :Streaming or :REST
# @option  [Hash<Symbol,String>]  keys          (nil) credential. key, secret, token
# @return [void]
def authenticate(screen_name:nil, type:nil, keys:nil)

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

# update media files
#
# @return [Array<Integer>] media_ids
def upload(files, rest)
  media_ids = []
  files.each do |media_filename|
    media_ids << rest.upload(File.new(media_filename))
  end
  media_ids
end


# tweets how many people in club room with images
#
# @param [Twitter::REST::Client]  rest      rest object
# @param [Twitter::Tweet]         status    status to reply
# @param [String]                 life_area headcount of life area
# @param [String]                 work_area headcount of work area
# @param [String]                 filename  filename to upload
# @param [String]                 dirs      dirs that has file to upload
# @param [Symbol]                 type      media file type
#
# @return  [void]
def rogy_watch(rest, status, life_area, work_area, filename, dirs, type: :png)
  media_ids = upload(dirs.map{|dir| "#{dir}/#{filename}.#{type}"}, rest)
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

