#include <string.h>
#include <windows.h>
#include "../zlib/zlib.h"

#define CHUNK 16384

extern "C" __declspec(dllexport) int __cdecl Compress(unsigned char* buffer, int bufferLength, unsigned char* writeBuffer, int writeBufferLength, bool good){
	int size = 0;
	int ret;
    unsigned have;
	size_t size_left = bufferLength;

    z_stream strm;
    unsigned char in[CHUNK];
    unsigned char out[CHUNK];

    strm.zalloc = Z_NULL;
    strm.zfree = Z_NULL;
    strm.opaque = Z_NULL;
	ret = deflateInit(&strm, (good) ? Z_BEST_COMPRESSION : Z_DEFAULT_COMPRESSION);
    if (ret != Z_OK)
		return 0;

    // compress until end of file //
    do {
		strm.avail_in = min(CHUNK, size_left);
		memcpy_s(in, strm.avail_in, buffer + bufferLength - size_left, strm.avail_in); 
		size_left -= strm.avail_in;

        strm.next_in = in;

        // run deflate() on input until output buffer not full, finish
        //  compression if all of source has been read in *
        do {
            strm.avail_out = CHUNK;
            strm.next_out = out;
            ret = deflate(&strm, Z_SYNC_FLUSH);    // no bad return value //
            have = CHUNK - strm.avail_out;
			memcpy_s(writeBuffer+size, have, out, have);
			size += have;
			if(size > writeBufferLength) return 0;
        } while (strm.avail_out == 0);

        // done when last data in file processed 
    } while (size_left);

    // clean up and return 
    (void)deflateEnd(&strm);

	return size;
}