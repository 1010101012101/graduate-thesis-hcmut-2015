@echo off
echo Training %2 problem (%1)...
if not exist "result\%1\Models" mkdir "result\%1\Models"
..\projects\emr-coreference-resolution\Output\Debug\%4\feat-train.exe -d result\%1\Features\%2.prb -i %2 -o result\%1\Models -l 1 %~3