#!/bin/bash

echo "Hello, world!"
# URL="https://api.github.com/repos/gortegaMS/loderunner/pulls/44/files"
# URL="https://api.github.com/repos/gortegaMS/loderunner/commits/ce867ecbf99e72db4bd0c86494048e55f022d29d"
URL="https://api.github.com/repos/gortegaMS/loderunner/commits/7e8912b8b31ea654ec855265ef6bd31864046778"
echo $URL
FILES=$(curl -s -X GET -G $URL | jq -r '. | .files')
# echo $FILES
if [[ "$FILES" == *"src/LodeRunner/"* ]] ; then
    echo "LodeRunner source code changed!!"\
else
echo "LodeRunner  DID NOT CHANGE!"
fi
         
if [[ "$FILES" == *"src/LodeRunner.API/"* ]] ; then
    echo "LodeRunner.API source code changed!!"
else
echo "LodeRunner api DID NOT CHANGE!"
fi

if [[ "$FILES" == *"src/LodeRunner.Core/"* || "$FILES" == *"src/LodeRunner.Data/"* ]] ; then
    echo "LodeRunner Libraries source code changed!!"
else
    echo "LodeRunner Lib DID NOT CHANGE!"
fi

if [[ "$FILES" == *"src/LodeRunner.UI/"* ]] ; then
    echo "LodeRunner.UI source code changed!!"
else
echo "LodeRunner Ui DID NOT CHANGE!"
fi

if [[ "$FILES" == *".github/workflows/"*".yaml"* ]]; then
    echo "LodeRunner Workflow changed!!"
else
    echo "LodeRunner Workflow DID NOT CHECHANGENGE!"
fi