#!/bin/bash

if [ -z "$LR_COL" ] && [ -z "$LR_DB" ] && [ -z "$LR_KEY" ] && [ -z "$LR_URL" ]
then
  echo "Please set all env variables for LodeRunner secrets (LR_COL, LR_DB, LR_KEY, LR_URL)"
else
  if [ -f ~/.lr.env ]
  then
    if [ "$#" = 0 ] || [ $1 != "-y" ]
    then
      read -p ".lr.env already exists. Do you want to remove? (y/n) " response

      if ! [[ $response =~ [yY] ]]
      then
        echo "Please move or delete ~/.lr.env and rerun the script."
        exit 1;
      fi
    fi
  fi

  echo '#!/bin/bash' > ~/.lr.env
  echo '' >> ~/.lr.env

  IFS=$'\n'

  for var in $(env | grep -E 'LR_' | sort | sed "s/=/='/g")
  do
    echo "export ${var}'" >> ~/.lr.env
  done

  cat ~/.lr.env
fi
