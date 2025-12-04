' ===============================================================================
' Locale English Resources
' This file is auto-generated for demonstration purposes.
' In production, modify the source .resx file instead.
' ===============================================================================

Imports System
Imports System.Globalization
Imports System.Resources

Namespace My.Resources
    
    ''' <summary>
    '''   A strongly-typed resource class for looking up localized strings, etc.
    ''' </summary>
    Friend Module Resources
        
        Private resourceMan As ResourceManager
        Private resourceCulture As CultureInfo
        
        ''' <summary>
        '''   Returns the cached ResourceManager instance used by this class.
        ''' </summary>
        Friend ReadOnly Property ResourceManager() As ResourceManager
            Get
                If resourceMan Is Nothing Then
                    resourceMan = New ResourceManager("Locale.Resources", GetType(Resources).Assembly)
                End If
                Return resourceMan
            End Get
        End Property
        
        ''' <summary>
        '''   The name of the application
        ''' </summary>
        Friend ReadOnly Property App_Name() As String
            Get
                Return ResourceManager.GetString("App_Name", resourceCulture)
            End Get
        End Property
        
        ''' <summary>
        '''   The description of the application
        ''' </summary>
        Friend ReadOnly Property App_Description() As String
            Get
                Return ResourceManager.GetString("App_Description", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Common_OK() As String
            Get
                Return ResourceManager.GetString("Common_OK", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Common_Cancel() As String
            Get
                Return ResourceManager.GetString("Common_Cancel", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Common_Save() As String
            Get
                Return ResourceManager.GetString("Common_Save", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Common_Delete() As String
            Get
                Return ResourceManager.GetString("Common_Delete", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Common_Edit() As String
            Get
                Return ResourceManager.GetString("Common_Edit", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Common_Close() As String
            Get
                Return ResourceManager.GetString("Common_Close", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Common_Loading() As String
            Get
                Return ResourceManager.GetString("Common_Loading", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Common_Error() As String
            Get
                Return ResourceManager.GetString("Common_Error", resourceCulture)
            End Get
        End Property
        
        ''' <summary>
        '''   Main page title
        ''' </summary>
        Friend ReadOnly Property Home_Title() As String
            Get
                Return ResourceManager.GetString("Home_Title", resourceCulture)
            End Get
        End Property
        
        ''' <summary>
        '''   Main page subtitle
        ''' </summary>
        Friend ReadOnly Property Home_Subtitle() As String
            Get
                Return ResourceManager.GetString("Home_Subtitle", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Home_GetStarted() As String
            Get
                Return ResourceManager.GetString("Home_GetStarted", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Messages_Success() As String
            Get
                Return ResourceManager.GetString("Messages_Success", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Messages_Warning() As String
            Get
                Return ResourceManager.GetString("Messages_Warning", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Messages_ConfirmDelete() As String
            Get
                Return ResourceManager.GetString("Messages_ConfirmDelete", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Messages_ItemNotFound() As String
            Get
                Return ResourceManager.GetString("Messages_ItemNotFound", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Validation_Required() As String
            Get
                Return ResourceManager.GetString("Validation_Required", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Validation_Email() As String
            Get
                Return ResourceManager.GetString("Validation_Email", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Validation_MinLength() As String
            Get
                Return ResourceManager.GetString("Validation_MinLength", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Validation_MaxLength() As String
            Get
                Return ResourceManager.GetString("Validation_MaxLength", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Navigation_Home() As String
            Get
                Return ResourceManager.GetString("Navigation_Home", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Navigation_Settings() As String
            Get
                Return ResourceManager.GetString("Navigation_Settings", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Navigation_Help() As String
            Get
                Return ResourceManager.GetString("Navigation_Help", resourceCulture)
            End Get
        End Property
        
        Friend ReadOnly Property Navigation_About() As String
            Get
                Return ResourceManager.GetString("Navigation_About", resourceCulture)
            End Get
        End Property
        
    End Module
    
End Namespace
