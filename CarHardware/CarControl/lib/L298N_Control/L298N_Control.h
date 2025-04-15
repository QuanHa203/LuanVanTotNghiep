#ifndef L298N_Control_H
#define L298N_Control_H

#include "L298N.h"
#include "IDevice_Control.h"


class L298N_Control : public IDevice_Control
{
private:
    L298N &_l298nA;
    L298N &_l298nB;

public:    
    L298N_Control(L298N &l298nA, L298N &l298nB);
    void turnLeft() override;
    void turnRight() override;
    void forward() override;
    void backward() override;
    void brake() override;
};

#endif