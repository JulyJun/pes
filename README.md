# Personal electricity saver

[KR README](https://github.com/JulyJun/pes/blob/main/READMEko.md)

### Description
 electricity saver on personal humidifier


 
The machine will only turn on with following circumstances

- mostly at night time
- no cloudy or rainy
- room humidity is lower than my custom set and will be trying to keep average
- room temperature is high

* * *

### Hardwares
- target humidifier
- STM 32 board (Nucleo-F103RB)
- Design State Machine for MCU
- temperature/humidity sensor (ETH-01DV)
- CO2 gas censor (RX-9)
- 1ch relay module (SZH-EK082)
- MQ gas censor
- ~ethernet/wifi module~
- I2C 16X2 lcd module (optional)
- PIR motion sensor (optional)
- Magnetic Door contact (optional)

 * * *

### Software use
- CubeIDE (C/C++ - main on/off logics)
- MySQL (DB)
- .Net framework (C# - controler)
- Kicad (trying to make my own PCB)

* * *

## Plan

1. Test hardware modules on Arduino (hardware defect check)
2. Module test and Assemble modules on STM32 MCU
3. build main logic on MCU
4. Design DB
5. build DB
6. build controller that contains following
   - CO2 gas graph
   - MQ gas graph
   - temp graph
   - gas graph
   - manual on/off control button
   - compare past data and today to inform human actions
7. PCB Design
8. PCB custom order
