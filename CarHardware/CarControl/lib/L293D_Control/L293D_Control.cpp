#include "L293D_Control.h"

L293D_Control::L293D_Control(uint8_t dirLatch, uint8_t dirSer, uint8_t dirEn, uint8_t dirClk, uint8_t pwm0A, uint8_t pwm0B, uint8_t pwm2A, uint8_t pwm2B) 
                        : _dirLatch(dirLatch), _dirSer(dirSer), _dirEn(dirEn), _dirClk(dirClk), _pwm0A(pwm0A), _pwm0B(pwm0B), _pwm2A(pwm2A), _pwm2B(pwm2B)
{
    pinMode(_dirLatch, OUTPUT);
    pinMode(_dirSer, OUTPUT);
    pinMode(_dirEn, OUTPUT);
    pinMode(_dirClk, OUTPUT);
    pinMode(_pwm0A, OUTPUT);
    pinMode(_pwm0B, OUTPUT);
    pinMode(_pwm2A, OUTPUT);
    pinMode(_pwm2B, OUTPUT);

    digitalWrite(_dirEn, LOW);
    digitalWrite(_pwm0A, HIGH);
    digitalWrite(_pwm0B, HIGH);

    digitalWrite(_pwm2A, HIGH);
    digitalWrite(_pwm2B, HIGH);
}

void L293D_Control::forward() {
    // 1001 1100
    digitalWrite(_dirLatch, LOW);
    lowBit();
    lowBit();
    highBit();
    highBit();
    
    highBit();
    lowBit();
    lowBit();
    highBit();
    digitalWrite(_dirLatch, HIGH);
}

void L293D_Control::backward() {    
    // 0110 0011
    digitalWrite(_dirLatch, LOW);
    highBit();
    highBit();
    lowBit();
    lowBit();
    
    lowBit();
    highBit();
    highBit();
    lowBit();
    digitalWrite(_dirLatch, HIGH);
}

void L293D_Control::turnLeft() {
    // 0001 1011
    digitalWrite(_dirLatch, LOW);
    lowBit();
    lowBit();
    highBit();
    lowBit();
    
    lowBit();
    highBit();
    highBit();
    highBit();
    digitalWrite(_dirLatch, HIGH);
}

void L293D_Control::turnRight() {
    // 1110 0100
    digitalWrite(_dirLatch, LOW);
    highBit();
    highBit();
    lowBit();
    highBit();
    
    highBit();
    lowBit();
    lowBit();
    lowBit();
    digitalWrite(_dirLatch, HIGH);
}

void L293D_Control::brake() {
    digitalWrite(_dirLatch, LOW);
    lowBit();
    lowBit();
    lowBit();
    lowBit();

    lowBit();
    lowBit();
    lowBit();
    lowBit();
    digitalWrite(_dirLatch, HIGH);
}

void L293D_Control::highBit() {
    digitalWrite(_dirClk, LOW);
    digitalWrite(_dirSer, HIGH);
    digitalWrite(_dirClk, HIGH);
}

void L293D_Control::lowBit() {
    digitalWrite(_dirClk, LOW);
    digitalWrite(_dirSer, LOW);
    digitalWrite(_dirClk, HIGH);
}