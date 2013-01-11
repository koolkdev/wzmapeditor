#include "AES.h"

extern "C" __declspec(dllexport) void __cdecl Decrypt(unsigned char* buffer, int bufferLength, unsigned char* key, unsigned char* iv){

	AES* aes = new AES();
	aes->SetParameters(256);
	aes->StartEncryption(key);
	aes->DecryptOFB(buffer, iv, bufferLength);
}