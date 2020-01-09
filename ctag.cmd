rem
rem useage: ctag tag newtag oldtag
rem
git tag %1 %2
git tag -d %2
git push origin :refs/tags/%2
git push --tags
