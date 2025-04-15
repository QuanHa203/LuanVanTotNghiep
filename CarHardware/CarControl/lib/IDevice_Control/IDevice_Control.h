#ifndef IDevice_Control_H
#define IDevice_Control_H

class IDevice_Control
{
public:
    virtual void forward();
    virtual void backward();
    virtual void turnLeft();
    virtual void turnRight();
    virtual void brake();
};

#endif