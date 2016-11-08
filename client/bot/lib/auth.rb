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

