#!/usr/bin/env sh
set -eu

script_dir="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
repo_root="$(CDPATH= cd -- "$script_dir/.." && pwd)"
key_dir="$repo_root/.secrets/jwt"
mkdir -p "$key_dir"
umask 077
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out "$key_dir/private.pem"
openssl pkey -in "$key_dir/private.pem" -pubout -out "$key_dir/public.pem"
chmod 600 "$key_dir/private.pem"
chmod 644 "$key_dir/public.pem"
printf '%s\n' 'JWT development key pair generated under .secrets/jwt (private.pem, public.pem).'
