#!/bin/bash

remote_pack_file_url="https://gist.githubusercontent.com/tommasobertoni/b6908c192edafe1e3a50151e0ad72ea6/raw/6873553ea16b163525fb5a5032ff8627e2775532/pack.sh"
pack_file="pack.sh"

if ! test -f "$pack_file"; then
    echo "Downloading pack.sh..."
    wget -O $pack_file $remote_pack_file_url -q
fi

chmod a+x $pack_file
bash $pack_file -P "../src/NScatterGather/NScatterGather.csproj" -O "tommasobertoni" -L "../LICENSE" -I "../assets/logo/nscattergather-logo-128.png"
