#!/bin/bash

usage() {
    echo "usage:$0 [-cn|--wild-domain <wild-card-domain>]  [--cert-path <path>]  [-san|--domains <comma-separated-domains>]  [--cert-prefix <cert-file-prefix>]"
}

wild_domain_val=localhost
domain_names=${wild_domain_val}
cert_path=.
cert_prefix=nginx_cosmos
while [ $# -gt 0 ]; do
    opt="$1" value="$2"
    case "$opt" in
        -cn|--wild-domain)
            wild_domain_val="$value";;
        -san|--domains) # comma separated domain list
            domain_names="$value";;
        --cert-path) # cert output dir
            cert_path="$value";;
        --cert-prefix) # cert file prefix
            cert_prefix="$value";;
        *)
            usage
            exit 1;;
    esac
    shift 2
done

# echo $cert_path
# echo $wild_domain_val
# echo $domain_names
# echo $cert_prefix
IFS=',' domain_names_tkn=( $domain_names )
cert_dns_names=$(printf 'DNS:%s,' ${domain_names_tkn[@]} )
echo """Will generate certificates for the following domains (SAN):
    ${domain_names[@]}
And Common Name (CN): ${wild_domain_val}"
# exit 0
cert_full_path=${cert_path}/${cert_prefix}

echo "Removing old certificates"
sudo rm /usr/local/share/ca-certificates/${cert_prefix}.* /usr/share/ca-certificates/${cert_prefix}.* /etc/ssl/certs/${cert_prefix}.*

sudo update-ca-certificates

set -e
subjectives="/CN = ${wild_domain_val}, /O = Microsoft Corporation, /ST = Texas, /C = US, /L = Austin"
openssl req -x509 -newkey rsa:4096 -sha256 \
    -days 3650 -nodes -keyout "${cert_full_path}.key" \
    -out "${cert_full_path}.crt" -subj "/CN=${wild_domain_val}/O=Microsoft Corporation/ST=Texas/C=US/L=Austin"\
    -addext "subjectAltName=${cert_dns_names::-1}"

echo "Certs generated: ${cert_full_path}.key and ${cert_full_path}.crt file"
echo "Adding generated crt to ca-certificates"
sudo cp ${cert_full_path}.crt /usr/local/share/ca-certificates/
sudo cp ${cert_full_path}.crt /usr/share/ca-certificates/

echo "Updating ca-certificates"
sudo update-ca-certificates
