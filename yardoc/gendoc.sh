#!/bin/sh

# $1 output dir

set -x
cd $(dirname $0)

out=${1:-"$(git rev-parse --abbrev-ref @)/"}
yard -o "${out}"
