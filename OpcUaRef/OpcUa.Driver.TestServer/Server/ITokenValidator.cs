using Opc.Ua;

namespace OpcUa.Driver.TestServer;

public interface ITokenValidator
{
	IUserIdentity ValidateToken(IssuedIdentityToken issuedToken);
}