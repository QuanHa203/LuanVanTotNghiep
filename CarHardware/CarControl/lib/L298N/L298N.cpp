#include "L298N.h"

/// @brief
/// @param pinMotorA1
///         IN1 Pin of L298N
/// @param pinMotorA2
///         IN2 Pin of L298N
/// @param pinMotorB1
///         IN3 Pin of L298N
/// @param pinMotorB2
///         IN4 Pin of L298N
L298N::L298N(uint8_t pinMotorA1, uint8_t pinMotorA2, uint8_t pinMotorB1, uint8_t pinMotorB2) : _pinMotorA1(pinMotorA1), _pinMotorA2(pinMotorA2), _pinMotorB1(pinMotorB1), _pinMotorB2(pinMotorB2)
{
    pinMode(_pinMotorA1, OUTPUT);
    pinMode(_pinMotorA2, OUTPUT);
    pinMode(_pinMotorB1, OUTPUT);
    pinMode(_pinMotorB2, OUTPUT);
}

/// @brief Motor A is IN1 and IN2
void L298N::spinMotorAClockwise()
{
    digitalWrite(_pinMotorA1, HIGH);
    digitalWrite(_pinMotorA2, LOW);
}

/// @brief Motor A is IN1 and IN2
void L298N::spinMotorACounterClockwise()
{
    digitalWrite(_pinMotorA1, LOW);
    digitalWrite(_pinMotorA2, HIGH);
}

/// @brief Motor A is IN1 and IN2
void L298N::stopMotorA()
{
    digitalWrite(_pinMotorA1, LOW);
    digitalWrite(_pinMotorA2, LOW);
}

// -----------------------------------------------------------------

/// @brief Motor B is IN3 and IN4
void L298N::spinMotorBClockwise()
{
    digitalWrite(_pinMotorB1, HIGH);
    digitalWrite(_pinMotorB2, LOW);
}

/// @brief Motor B is IN3 and IN4
void L298N::spinMotorBCounterClockwise()
{
    digitalWrite(_pinMotorB1, LOW);
    digitalWrite(_pinMotorB2, HIGH);
}

/// @brief Motor B is IN3 and IN4
void L298N::stopMotorB()
{
    digitalWrite(_pinMotorB1, LOW);
    digitalWrite(_pinMotorB2, LOW);
}