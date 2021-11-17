package main

import (
	"crypto/rand"
	"crypto/rsa"
	"crypto/x509"
	"encoding/base64"
	"encoding/pem"
	"fmt"
	"net/http"
	"os"
	"strconv"
)

func makeHandler(offset int, privateKey *rsa.PrivateKey) func(http.ResponseWriter, *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		// read body
		body := make([]byte, r.ContentLength)
		r.Body.Read(body)

		// base64 decode
		cipherText, err := base64.StdEncoding.DecodeString(string(body))
		if err != nil {
			fmt.Fprintln(w, "base64 decode error")
			return
		}

		// decrypt
		rawData, err := rsa.DecryptPKCS1v15(rand.Reader, privateKey, cipherText)
		if err != nil {
			fmt.Fprintln(w, "decrypt error")
			return
		}

		// read the random number from client
		num, err := strconv.Atoi(string(rawData))
		if err != nil {
			fmt.Fprintln(w, "read number error")
			return
		}

		// return random number + offset to client
		num += offset
		fmt.Fprintln(w, strconv.Itoa(num))
	}
}

func load() (int, *rsa.PrivateKey, error) {
	var err error
	offset, err := strconv.Atoi(os.Getenv("OFFSET"))
	if err != nil {
		return offset, nil, fmt.Errorf("read OFFSET error %v", err)
	}

	// load key bytes
	keyFile, err := os.Open("app.pem")
	if err != nil {
		return offset, nil, fmt.Errorf("open key file error %v", err)
	}
	defer keyFile.Close()
	st, _ := keyFile.Stat()
	keyBytes := make([]byte, st.Size())
	keyFile.Read(keyBytes)

	// load key
	block, _ := pem.Decode(keyBytes)
	if block == nil {
		return offset, nil, fmt.Errorf("decode private key error %v", err)
	}

	privateKey, err := x509.ParsePKCS1PrivateKey(block.Bytes)
	if err != nil {
		return offset, privateKey, fmt.Errorf("parse private key error %v", err)
	}

	return offset, privateKey, nil
}

func main() {
	offset, privateKey, err := load()
	if err != nil {
		fmt.Println(err)
		os.Exit(1)
	}

	// start http server
	http.HandleFunc("/", makeHandler(offset, privateKey))
	http.ListenAndServe("0.0.0.0:8080", nil)
}
