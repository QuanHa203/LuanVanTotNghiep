#include "L298N.h"
#include "L298N_Control.h"

L298N_Control::L298N_Control(L298N &l298nA, L298N &l298nB) : _l298nA(l298nA), _l298nB(l298nB)
{ }

void L298N_Control::turnLeft()
{
    _l298nA.spinMotorACounterClockwise();
    _l298nA.spinMotorBCounterClockwise();

    _l298nB.spinMotorACounterClockwise();
    _l298nB.spinMotorBCounterClockwise();
}

void L298N_Control::turnRight()
{
    _l298nA.spinMotorAClockwise();
    _l298nA.spinMotorBClockwise();

    _l298nB.spinMotorAClockwise();
    _l298nB.spinMotorBClockwise();
}

void L298N_Control::forward()
{
    _l298nA.spinMotorAClockwise();
    _l298nA.spinMotorBCounterClockwise();

    _l298nB.spinMotorACounterClockwise();
    _l298nB.spinMotorBClockwise();
}

void L298N_Control::backward()
{
    _l298nA.spinMotorACounterClockwise();
    _l298nA.spinMotorBClockwise();

    _l298nB.spinMotorAClockwise();
    _l298nB.spinMotorBCounterClockwise();
}

void L298N_Control::brake() {
    _l298nA.stopMotorA();
    _l298nA.stopMotorB();

    _l298nB.stopMotorA();
    _l298nB.stopMotorB();
}