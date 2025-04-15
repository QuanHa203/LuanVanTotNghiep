#ifndef L298N_H
#define L298N_H

#include "Arduino.h"

class L298N
{
private:
    uint8_t _pinMotorA1, _pinMotorA2, _pinMotorB1, _pinMotorB2;

public:
    L298N(uint8_t pinMotorA1, uint8_t pinMotorA2, uint8_t pinMotorB1, uint8_t pinMotorB2);
    void spinMotorAClockwise();
    void spinMotorACounterClockwise();

    void spinMotorBClockwise();
    void spinMotorBCounterClockwise();
    void stopMotorA();
    void stopMotorB();
};


#endif