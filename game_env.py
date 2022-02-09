#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import base64
import time


key_n = rsa.PrivateKey.load_pkcs1('RSA_PRIVATE_KEY').n
key_d = rsa.PrivateKey.load_pkcs1('RSA_PRIVATE_KEY').d


def int_to_binary(x):
    bin_list = []
    while x != 0:
        bin_list.append(x % 2)
        x = x >> 1
    return bin_list


# compute a^d % n with Montgomery
def mod_exp(a, b, n):
    res = 1
    bin_list = int_to_binary(b)
    for i in bin_list:
        if i == 1:
            res = (res * a) % n
        a = (a * a) % n
    return res


def make_config(env_index, validity):
    now = time.time()
    now = int(now)
    exp = now + validity * 3600
    num = exp * 10 + env_index
    print(num)

    cipher = mod_exp(num, key_d, key_n)
    print(cipher)

    nbytes, rem = divmod(cipher.bit_length(), 8)
    if rem:
        nbytes += 1

    cipher_bytes = cipher.to_bytes(nbytes, 'little')
    cipher_text = base64.b64encode(cipher_bytes)
    print(cipher_text)
    return cipher_text


if __name__ == '__main__':
    make_config(1, 36)
