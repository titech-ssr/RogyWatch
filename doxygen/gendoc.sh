#!/bin/sh

# $1 output dir. generate output/html

set -x
cd $(dirname $0)

out=${1:-"$(git rev-parse --abbrev-ref @)"}
( cat Doxyfile; echo "OUTPUT_DIRECTORY = ${out}" ) | doxygen -
