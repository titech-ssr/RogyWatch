#!/bin/sh

# $1 output dir

out=${1:-"$(git rev-parse --abbrev-ref @)/"}
yard -o "${out}"
