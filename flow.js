// JavaScript source code
/***********************
**** REQUIRE MODULE*****
************************/
const mqtt = require('mqtt')
const faker = require('faker')
const { each, eachLimit } = require('async')
const request = require('request')

/***********************
**** INIT FAKE DATA*****
************************/
const API_ENDPOINT = 'http://localhost:3000/api'
const devices = []
const mqttClients = []
const Apis = {
    authorizedDevice: 'authorizeddevices',
    credential: 'devices/credentials'
}
const options = {
    baseUrl: API_ENDPOINT,
    json: true,
    headers: {
        'User-Agent': 'request',
        'Content-Type': 'application/json',
        Accept: 'application/json'
    }
}

const authorizedDevices = (number = 100) => {
    const ads = []
    for (let i = 0; i < number; i++) {
        ads.push({
        uuid: faker.random.uuid(),
        serialId: faker.random.uuid(),
        macAddress: faker.internet.mac(),
        model: faker.random.word()
        })
    }
    return ads
}
/************************************
**** STEP 0: PROVISIONING DEVICE*****
*************************************/
// Import Authorize devices list
const provisioningDevice = (body) => new Promise((resolve, reject) => {
    request.put({
        ...options,
        url: Apis.authorizedDevice,
        body
        }, function (error, response, body) {
        if (error) {
            return reject(error)
        }
        if (response && response.statusCode >= 400) {
            return reject(new Error(JSON.stringify(body)))
        }
        resolve()
    })
})

/************************************
**** STEP 1: GET MQTT CREDENTIAL*****
*************************************/
/* Example data:
{
  "endpoint": "localhost",
  "port": 1883,
  "protocol": "mqtt",
  "password": "uRusPZVbvb6bxLI",
  "token": "uRusPZVbvb6bxLI",
  "username": "1234",
  "clientId": "1234"
}
*/
const createMqttCredential = (body) => new Promise((resolve, reject) => {
    request.put({
        ...options,
        url: Apis.credential,
        body
        }, function (error, response, body) {
        if (error) {
            return reject(error)
        }
        if (response && response.statusCode >= 400) {
            return reject(new Error(JSON.stringify(body)))
        }
        resolve(body)
    })
})
/************************************
**** STEP 2: CONNECT TO MQTT BROKER**
*************************************/

/************************************
**** STEP 3: REPORT DEVICE INFO******
*************************************/
const reportInfo = (mqttClient) => {
  const topic = `bms/${mqttClient.options.clientId}/info`
  const payload = {
    cloudVersion: faker.random.number(),
    localVersion: faker.random.number(),
    system: {
      id: faker.random.uuid(),
      name: faker.random.word(),
      culture: faker.random.number(),
      timezone: 'Asia/Ho_Chi_Minh',
      r1: faker.random.number(),
      r2: faker.random.number(),
      vscale: faker.random.number(),
      address16: faker.random.number(),
      address24: faker.random.number(),
      address32: faker.random.number(),
      address40: faker.random.number()
    },
    unit: {
      id: faker.random.uuid(),
      name: faker.random.words(3),
      address: faker.random.number(),
      blkCapacity: faker.random.number(),
      refreshDuration: faker.random.number(),
      enable: faker.random.boolean(),
      operationMode: faker.random.number(),
      voltageLevel: faker.random.number(),
      deviceType: faker.random.word(),
      cableType: faker.random.word(),
      capacity: faker.random.number()
    },
    block: {
      r1: faker.random.number(),
      r2: faker.random.number(),
      vscale: faker.random.number(),
      address16: faker.random.number(),
      address24: faker.random.number(),
      address32: faker.random.number(),
      address40: faker.random.number(),
      enable: faker.random.boolean(),
      operationMode: faker.random.number(),
      voltageLevel: faker.random.number(),
      deviceType: faker.random.word(),
      cableType: faker.random.word(),
      capacity: faker.random.number()
    },
    timestamp: Math.round(Date.now() / 1000)
  }
  mqttClient.publish(topic, JSON.stringify(payload), function () {
    console.log(`${mqttClient.options.clientId} Publishing topic ${topic}`)
  })
}
/************************************
**** STEP 4: REPORT DEVICE STATUS****
*************************************/
const reportStatus = (mqttClient) => {
  const topic = `bms/${mqttClient.options.clientId}/state`
  const payload = {
    data: [
      {
        enable: faker.random.boolean(),
        r1: faker.random.number(),
        r2: faker.random.number(),
        v0: faker.random.number(),
        v1: faker.random.number(),
        v2: faker.random.number(),
        e: faker.random.number(),
        r: faker.random.number(),
        t: faker.random.number(),
        soc: faker.random.number(),
        totalDischargeSecond: faker.random.number(),
        totalDischargeCycle: faker.random.number(),
        expectedLifeTimeRest: faker.random.number(),
        vscale: faker.random.number(),
        address16: faker.random.number(),
        address24: faker.random.number(),
        address32: faker.random.number(),
        address40: faker.random.number(),
        operationMode: faker.random.number(),
        refreshDuration: faker.random.number(),
        voltageLevel: faker.random.number(),
        deviceType: faker.random.word(),
        cableType: faker.random.word(),
        capacity: faker.random.number(),
        unit: {
          name: faker.random.words(3),
          address: faker.random.number(),
          blkCapacity: faker.random.number()
        }
      }
    ],
    timestamp: Math.round(Date.now() / 1000)
  }
  mqttClient.publish(topic, JSON.stringify(payload), function () {
    console.log(`${mqttClient.options.clientId} Publishing topic ${topic}`)
  })
}
/************************************
*********** MAIN FUNTION ************
*************************************/
const app = async () => {
  const ads = authorizedDevices(1)
  console.log(`
/************************************
**** STEP 0: PROVISIONING DEVICE*****
*************************************/`)
  await provisioningDevice(ads)
  console.log(`
/************************************
**** STEP 1: GET MQTT CREDENTIAL*****
*************************************/
`)
  await eachLimit(ads, 5, async (ad) => {
    const creds = await createMqttCredential({
      uuid: ad.uuid,
      macAddress: ad.macAddress
    })
    devices.push(creds)
  })
  console.log(`
/************************************
**** STEP 2: CONNECT TO MQTT BROKER**
*************************************/`)
  await each(devices, async (device) => {
    const mqttOptions = {
      port: device.port,
      host: device.endpoint,
      protocol: device.protocol,
      protocolVersion: 4,
      clientId: device.clientId,
      rejectUnauthorized: true,
      clean_session: true,
      username: device.username,
      password: device.password,
      will: {
        topic: `bms/${device.clientId}/lastwill`,
        payload: `{
          "online": false,
          "timestamp": ${Date.now() / 1000}
        }`
      },
      keepalive: 20
    }
    const mqttClient = mqtt.connect(mqttOptions)

    mqttClients.push(mqttClient)
  })
  console.log(`
/************************************
**** STEP 3: REPORT DEVICE INFO******
*************************************/
`)
  await each(mqttClients, async (mqttClient) => {
    mqttClient.on('connect', function (data, err) {
      if (err) {
        console.error(err)
        return
      }
      console.log(mqttClient.options.clientId, ' connected')
      setInterval(() => {
        reportStatus(mqttClient)
      }, 2 * 1000)
      setInterval(() => {
        reportInfo(mqttClient)
      }, 2 * 1000)
    })
    mqttClient.on('message', function (topic, payload) {
      console.log(`${mqttClient.options.clientId} Receiving MQTT MESSAGE: ${topic}`)
    })
    mqttClient.on('error', function (err) {
      console.log('<<<<<<<<<<<<<mqtt error>>>>>>>>>>>>>', err)
    })
    mqttClient.on('disconnect', function (err) {
      console.log('<<<<<<<<<<<<<mqtt disconnect>>>>>>>>>>>>>', err)
    })
    mqttClient.on('close', function () {
      console.log('<<<<<<<<<<<<<mqtt close>>>>>>>>>>>>>')
    })
  })
}
app()
