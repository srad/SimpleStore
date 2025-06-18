@echo off

git clone https://github.com/twbs/bootstrap.git --branch v5.3.1 --single-branch wwwroot/src/scss/lib/bootstrap
git clone --single-branch --branch v1.10.5 https://github.com/twbs/icons.git wwwroot/src/scss/lib/icons

exit