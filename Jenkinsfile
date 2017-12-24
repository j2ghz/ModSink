pipeline {
  agent {
    docker {
      image 'microsoft/dotnet:2.0.4-sdk-2.1.3'
      args '--user root'
    }
    
  }
  stages {
    stage('Restore') {
      steps {
        sh 'dotnet restore'
      }
    }
    stage('Process') {
      parallel {
        stage('Test') {
          steps {
            dir(path: 'src/ModSink.Common.Tests') {
              sh 'dotnet restore'
              sh 'dotnet xunit -xml result.xml'              
            }            
          }
          post {
            success {
              junit 'src/ModSink.Common.Tests/result.xml'
            }
          }
        }
        stage('Publish') {
          steps {
            sh 'dotnet pack -c Release --include-symbols /p:TargetFrameworks=netstandard2.0 src/ModSink.Common/ModSink.Common.csproj'
            archiveArtifacts 'src/ModSink.Common/bin/*.nupkg'
          }
        }
      }
    }
  }
}
