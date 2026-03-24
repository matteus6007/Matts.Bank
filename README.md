# Matt's Banking API

Banking API that allows customer's to deposit and withdraw money from their account. 

## Running the Application

### Locally

```
dotnet run --project ./src/MattsBank.Api
```

Navigate to http://localhost:5046/swagger/index.html to view Open API spec.

### Docker

```
docker-compose up --build
```

You can add custom certificates by setting the `CERT_FILE_PATH` - default is `.ca_certs` - and `CERT_FILE` arguments via environment variables.

Note: the certificate needs to be available in the docker build context.

```
$env:CERT_FILE="my-certificate.crt";docker-compose up --build
```

Navigate to http://localhost:1001/swagger/index.html to view Open API spec.

## Testing

```
dotnet test
```

## Error Handling

Returned in standard [Problem Details](https://www.rfc-editor.org/rfc/rfc9457.html) format.

Example:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Account not found.",
  "status": 404,
  "traceId": "00-43c01d5ef19b3ea6dedc0f58e5c1eec3-b058b5c9a7474a71-00"
}
```
