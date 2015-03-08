import socket
import json
import time
from termios import tcflush, TCIFLUSH
import sys
import Adafruit_BBIO.PWM as PWM
import Adafruit_BBIO.ADC as ADC
import Adafruit_BBIO.GPIO as GPIO

#set pins
frontright_pin = "P9_14"        #PWM pin
frontleft_pin = "P9_21"         #PWM pin
rearright_pin = "P8_46"         #PWM pin        #P9_22 is available PWM still but wasn't working for some reason
rearleft_pin = "P9_29"          #PWM pin

compressor_output_pin_high = "P8_18"  #GPIO pin
compressor_output_pin_low = "P8_17"   #GPIO pin

compressor_sensor = "P9_13"         #GPIO pin

jumpdrive_pin = "P9_23"                #GPIO pin

shooter_pin = "P8_13"              #PWM pin
conveyor_pin = "P8_34"             #PWM pin
spinner_pin = "P8_45"              #PWM pin (constantly spinning regardless of anything else) (may want to change this)
#end of pin assignments

#create jumpdrive state
jumpstate = 0      #0 is... I forget... actually I haven't set it yet lets say 0 is mechanum mode
#whenever we get a toggle signal we need to change this
#this is more complicated than you would think
#need current state and past state variables to determine switch
lastjmpsig = 0      #initialize the last state to be 0
#after code always update this with last JumpDrive signal sent
#if(!lastjmpsig && JumpDrive) then toggle
#lastjmpsig = JumpDrive
#toggle()
# if(jumpstate)
#       jumpstate = 0
# else
#       jumpstate = 1

#initialize jumpdrive input
JumpDrive = 0       #please set to 1 later, well hmm...

#start PWMs at neutral
#print "pointa"
PWM.start(frontright_pin,7.5,50,0)  #frequency of 50Hz results in 20ms period 
PWM.start(frontleft_pin,7.5,50,0)  #for Talon Motor Controllers pulses should last 1-2ms
PWM.start(rearright_pin,7.5,50,0)  #which is 5-10% duty cycle (5 = backwards, 7.5 = neutral, 10 = forward)
PWM.start(rearleft_pin,7.5,50,0)  #figure out which pins are PWM able
PWM.start(shooter_pin,7.5,50,0)  #the 7 PWM signals are the 4 wheels, shooter, conveyor, spinner
PWM.start(conveyor_pin,7.5,50,0)  #the conveyor is a boolean, either 7.5% or 10% (we will change the value) (easier to make PWM regularly)
PWM.start(spinner_pin,10,50,0)  #the spinner is always on full go (may need to adjust the value)
#print "pointb"

GPIO.setup(compressor_sensor, GPIO.IN)              #this is the input that lets us know if it is fully compressed
GPIO.setup(compressor_output_pin_high, GPIO.OUT)    #i dont understand this output (copied from Ahmed)
GPIO.setup(compressor_output_pin_low, GPIO.OUT)     #i dont understand this output (copied from Ahmed)
GPIO.setup(jumpdrive_pin, GPIO.OUT)                 #is just the jumpstate

#note Ahmed uses threading which I don't understand so I'm
#just gonna throw everything into the big while loop down there
#the readability is lessened but it will still work

TCP_IP = '192.168.7.2'
TCP_PORT = 5005
BUFFER_SIZE = 512  # Normally 1024, but we want fast response

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind((TCP_IP, TCP_PORT))
s.listen(1)
    
    #PWM.set_duty_cycle(pin, pwmval) use this to change PWM duty cycle as we go along
    
try:    
    while 1:
    
        conn, addr = s.accept()
        print 'Connection address:', addr
        while 1:
            data = conn.recv(BUFFER_SIZE)
            if not data: break
            #print "received data:", data
            controllerdata = json.loads(str(data))
            #for k,v in controllerdata.items(): 
                #print str(k) + " " + str(v)
            #here is where we set all of the pins
            #do jumpstate first then PWM after
            #print "lastjmpsig", lastjmpsig
            #print "JumpDrive", controllerdata['JumpDrive']    #this is a variable from controller data you made this one
            if (not(lastjmpsig) and controllerdata['JumpDrive']): 
                #print "toggle jumpstate"
                if jumpstate:
                    jumpstate = 0
                else:
                    jumpstate = 1
            lastjmpsig = controllerdata['JumpDrive']
            #print jumpstate
            if jumpstate:
                GPIO.output(jumpdrive_pin, GPIO.HIGH)
            else:
                GPIO.output(jumpdrive_pin, GPIO.LOW)
            
            #set compressor stuff (copied from Ahmed's code)
            var = bool (GPIO.input(compressor_sensor))
            if var:
                #print "DETECTED"
                GPIO.output(compressor_output_pin_high, GPIO.LOW)
                GPIO.output(compressor_output_pin_low, GPIO.LOW)
            else:
                #print "TURN ON"
                GPIO.output(compressor_output_pin_high, GPIO.HIGH)
                GPIO.output(compressor_output_pin_low, GPIO.LOW)
            #now compressor is set and also jumpdrive is set
            #next are the 6 PWM pins
            
            #find the duty cycle passed in based on jumpstate
            #map the PWM val to 5 to 10 duty cycle
            #need to have GUI variable be from 0 to 255 (alter that)
            #then divide by 255 and multiply by the range (2.5) and add 7.5
            #so write this code with that assumption I guess
            #well actually we have data from 30-90 so if we multiply by 2.. hmm
            #okay i fixed it in the code (it was super easy)
            if jumpstate:
                #assign duty cycles for reg drive
                frduty = float(float(controllerdata['FrontRight'])/50.8 + 5.00)
                flduty = float(float(controllerdata['FrontLeft'])/50.8 + 5.00)
                rrduty = float(float(controllerdata['RearRight'])/50.8 + 5.00)
                rlduty = float(float(controllerdata['RearLeft'])/50.8 + 5.00)

            else:
                #assign duty cycles for mech drive
                frduty = float(float(controllerdata['MechFR'])/50.8 + 5.00) #50.8 = 127/2.5
                flduty = float(float(controllerdata['MechFL'])/50.8 + 5.00) #which is pwmstop(fromGUI)/pwmrange(here)
                rrduty = float(float(controllerdata['MechRR'])/50.8 + 5.00) #converts it to proper value
                rlduty = float(float(controllerdata['MechRL'])/50.8 + 5.00) 

                
            shduty = float(float(controllerdata['Shooter']/50.8) + 5.00)
            cvduty = float(float(controllerdata['Conveyor']/50.8) + 5.00)   
            
            PWM.set_duty_cycle(frontright_pin, frduty)
            PWM.set_duty_cycle(frontleft_pin, flduty)
            PWM.set_duty_cycle(rearright_pin, rrduty)
            PWM.set_duty_cycle(rearleft_pin, rlduty)
            PWM.set_duty_cycle(shooter_pin, shduty)
            PWM.set_duty_cycle(conveyor_pin, cvduty)
            
            print "frduty", frduty
            print "flduty", flduty
            print "rrduty", rrduty
            print "rlduty", rlduty
            print "shduty", shduty
            print "cvduty", cvduty
            print "jumpstate", jumpstate
            
            conn.send('ack')
        conn.close()
        tcflush(sys.stdin, TCIFLUSH)

except KeyboardInterrupt:       #when user inputs ctrl + c
    print ""
    print "Exit"
    PWM.set_duty_cycle(frontright_pin,7.5)  #set pwm values to neutral
    PWM.set_duty_cycle(frontleft_pin,7.5)
    PWM.set_duty_cycle(rearright_pin,7.5)
    PWM.set_duty_cycle(rearleft_pin,7.5)
    PWM.set_duty_cycle(shooter_pin,7.5)
    PWM.set_duty_cycle(conveyor_pin,7.5)
