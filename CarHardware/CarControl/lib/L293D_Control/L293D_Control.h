#ifndef L293D_Control_H
#define L293D_Control_H

#include "Arduino.h"
#include "IDevice_Control.h"

class L293D_Control : public IDevice_Control
{
private:
    uint8_t _dirLatch, _dirSer, _dirEn, _dirClk, _pwm0A, _pwm0B, _pwm2A, _pwm2B;
    void lowBit();
    void highBit();

public:
    L293D_Control(uint8_t dirLatch, uint8_t dirSer, uint8_t dirEn, uint8_t dirClk, uint8_t pwm0A, uint8_t pwm0B, uint8_t pwm2A, uint8_t pwm2B);
    void forward() override;
    void backward() override;
    void turnLeft() override;
    void turnRight() override;
    void brake() override;
};

#endif