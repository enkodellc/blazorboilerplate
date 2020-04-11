rem
rem useage: ctag newtag oldtag
rem
git tag %1 %2
git tag -d %2
git push origin :refs/tags/%2
git push --tags
