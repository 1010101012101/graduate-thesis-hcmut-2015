@echo off
echo Extracting features (%1)...
..\projects\emr-coreference-resolution\Output\Debug\%5\feat-extract.exe -d ..\dataset\i2b2_Train -m Train -o result\%1\Features -i %~2 -g %3 -u %4