pipeline {
  agent any
  stages {
    stage('Checkout') {
      steps {
        git(url: 'https://github.com/Jaensche/PhonieCore.git', branch: 'master', poll: true)
      }
    }

    stage('Build') {
      steps {
        bat 'nuget restore'
        bat 'dotnet msbuild'
      }
    }

    stage('Artifacts') {
      steps {
        archiveArtifacts 'PhonieCore/bin/Debug/netcoreapp3.1/*'
      }
    }

  }
}