#include <stdint.h>
#include <intrin.h>

void insw(uint16_t port,void* ptr,uint64_t count) {
	__inwordstring(port, ptr, count);
}

void outsw(uint16_t port, void* ptr, uint64_t count) {
	__outwordstring(port, ptr, count);
}

void hlt() {
	__halt();
}

void invlpg(void* ptr) {
	__invlpg(ptr);
}

uint64_t readCR2() {
	return __readcr2();
}

void writeCR3(uint64_t value) {
	__writecr3(value);
}

uint8_t in8(uint16_t port) {
	return __inbyte(port);
}

uint16_t in16(uint16_t port) {
	return __inword(port);
}

uint32_t in32(uint16_t port) {
	return __indword(port);
}

void out8(uint16_t port, uint8_t value) {
	__outbyte(port, value);
}

void out16(uint16_t port, uint16_t value) {
	__outword(port, value);
}

void out32(uint16_t port, uint32_t value) {
	__outdword(port, value);
}