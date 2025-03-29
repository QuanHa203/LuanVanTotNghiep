#ifndef Device_Control_H
#define Device_Control_H


#include "L298N_Control.h"

class Device_Control
{
private:
    L298N_Control &_l298nA;
    L298N_Control &_l298nB;

public:    
    Device_Control(L298N_Control &l298nA, L298N_Control &l298nB);
    void turnLeft();
    void turnRight();
    void forward();
    void backward();
    void brake();
};

#endif