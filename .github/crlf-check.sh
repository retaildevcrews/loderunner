#!/bin/bash
find . -type f -iname '*.cs' -exec file -- {} ';' | grep CRLF