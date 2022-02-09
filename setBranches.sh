#!/bin/bash

performUpdates() {
  echo ""
  echo "Fetching submodules recursively"
  git submodule foreach git fetch --all --tags

  echo ""
  echo "Updating submodules recursively"
  git submodule update --init --recursive --remote

  echo ""
  echo "Checking out default branches"
  _OPWD=$(pwd)
  for MODULE in $_GIT_SUBMODULES; do
    echo "  - Checking out the default branch for ${MODULE}"
    if [[ "${MODULE}" == "wiki" ]]
    then
      _BRANCH=master
    else
      _BRANCH=main
    fi
    cd "${MODULE}" && \
      git checkout --track origin/${_BRANCH} -B ${_BRANCH} && \
      cd "${_OPWD}"
  done
}

echo ""
echo "Submodule Update:"
echo "-----------------"
echo "Getting list of modules"
export _GIT_SUBMODULES=$(git submodule | awk '{ print $2; }')
performUpdates
echo ""
echo "-----------------"
echo "Update complete"
echo ""
