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

var offset int
var privateKey *rsa.PrivateKey

func handler(w http.ResponseWriter, r *http.Request) {
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

func load() error {
	var err error
	offset, err = strconv.Atoi(os.Getenv("OFFSET"))
	if err != nil {
		return fmt.Errorf("read OFFSET error %v", err)
	}

	// load key bytes
	keyFile, err := os.Open("app.pem")
	if err != nil {
		return fmt.Errorf("open key file error %v", err)
	}
	defer keyFile.Close()
	st, _ := keyFile.Stat()
	keyBytes := make([]byte, st.Size())
	keyFile.Read(keyBytes)

	// load key
	block, _ := pem.Decode(keyBytes)
	if block == nil {
		return fmt.Errorf("decode private key error %v", err)
	}

	privateKey, err = x509.ParsePKCS1PrivateKey(block.Bytes)
	if err != nil {
		return fmt.Errorf("parse private key error %v", err)
	}

	return nil
}

func main() {
	err := load()
	if err != nil {
		fmt.Println(err)
		os.Exit(1)
	}

	// start http server
	http.HandleFunc("/", handler)
	http.ListenAndServe("0.0.0.0:8080", nil)
}
