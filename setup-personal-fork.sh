#!/bin/bash
if [[ $# -ne 1 ]]
then
  echo "usage: $0 {github username}"
  exit 1
fi

setupFork() {
  _USERNAME=$1
  echo ""
  echo "Setting origins for all submodules"
  _OPWD=$(pwd)
  for MODULE in $_GIT_SUBMODULES; do
    echo ""
    echo "  - entering '${MODULE}'"
    cd "${_OPWD}" && cd "${MODULE}" # avoid os path separator issues
    echo "  - before:"
    git remote -v
    echo "  - Setting origins for ${MODULE}"
    git remote rename origin upstream
    git remote add origin https://github.com/${_USERNAME}/${MODULE}
    git remote set-url --push origin git@github.com:${_USERNAME}/${MODULE}
    echo ""
    echo "  - after:"
    git remote -v
  done
  echo "returning to ${_OPWD}"
  cd "${_OPWD}"
}

echo ""
echo "Set up Personal Fork Origin:"
echo "-----------------"
echo "Getting list of modules"
export _GIT_SUBMODULES=$(git submodule | awk '{ print $2; }')
setupFork "$1"
echo ""
echo "-----------------"
echo "Done"
echo ""
