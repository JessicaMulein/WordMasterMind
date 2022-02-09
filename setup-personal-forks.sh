#!/bin/bash
if [[ $# -ne 1 ]]
then
  echo "usage: $0 {github username}"
  exit 1
fi

setupFork()
{
  _USERNAME=$1
  _MODULE=$2
  _FROMDIR=$3
  _PATH=$(basename "$_MODULE")
  echo ""
  echo "  - entering '${_MODULE}'"
  cd "${_FROMDIR}" && cd "${_MODULE}"
  echo "  - before:"
  git remote -v
  echo "  - Setting origins for ${_MODULE}"
  git remote rename origin upstream
  git remote set-url --push upstream git@github.com:WordMasterMind/${_PATH}.git
  git remote add origin https://github.com/${_USERNAME}/${_PATH}.git
  git remote set-url --push origin git@github.com:${_USERNAME}/${_PATH}.git
  echo ""
  echo "  - after:"
  git remote -v
}

setupForks() {
  _USERNAME=$1
  _FROMDIR=$2
  echo ""
  echo "Setting origins for all submodules"
  for MODULE in $_GIT_SUBMODULES; do
    if [[ "${MODULE}" == "wiki" ]]
    then
      echo "Skipping wiki"
      continue
    fi
    setupFork "${_USERNAME}" "${MODULE}" "${_FROMDIR}"
  done
}

echo ""
echo "Set up Personal Fork Origin:"
echo "-----------------"
echo "Getting list of modules"
export _GIT_SUBMODULES=$(git submodule | awk '{ print $2; }')
_OPWD=$(pwd)
setupFork "$1" WordMasterMind ".."
cd "${_OPWD}" && setupForks "$1" "${_OPWD}"
echo ""
echo "-----------------"
echo "Done"
echo ""
