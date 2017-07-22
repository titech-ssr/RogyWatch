#!/bin/bash

# rbenv
export RBENV_ROOT="/usr/local/rbenv"
export PATH="/usr/local/rbenv/bin:$PATH"
eval "$(rbenv init -)"

cd ~/RogyWatch/client/bot
./launch.rb
