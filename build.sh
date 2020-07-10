mkdir ../IF_Build
rsync -a Assets ../IF_Build
rsync -a Library ../IF_Build
rsync -a Packages ../IF_Build
rsync -a ProjectSettings ../IF_Build
(
	cd ../IF_BUILD
	rm -rf !(.git)
	Unity -quit -batchmode -executeMethod MultiBuild.Builder.Build
	cd Build
	mv "Android/Idle Factory" "Android/Idle Factory.apk"
	if [ ! -z "$1" ]
	  then
		git add .
		git commit -m "$1"
		git push
	fi
)